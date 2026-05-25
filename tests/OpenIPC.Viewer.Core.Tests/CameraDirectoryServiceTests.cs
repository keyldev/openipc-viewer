using OpenIPC.Viewer.Core.Entities;
using OpenIPC.Viewer.Core.Services;
using OpenIPC.Viewer.Core.Tests.Fakes;

namespace OpenIPC.Viewer.Core.Tests;

public sealed class CameraDirectoryServiceTests
{
    private static (CameraDirectoryService Service, InMemoryCameraRepository Cameras, InMemorySecretsStore Secrets) Create()
    {
        var cameras = new InMemoryCameraRepository();
        var groups = new InMemoryGroupRepository();
        var secrets = new InMemorySecretsStore();
        return (new CameraDirectoryService(cameras, groups, secrets), cameras, secrets);
    }

    private static NewCameraRequest BuildRequest(string name = "Front door", string user = "admin", string pass = "12345") =>
        new(
            Name: name,
            Host: "192.168.1.10",
            HttpPort: 80,
            OnvifPort: null,
            RtspMainUri: new Uri("rtsp://192.168.1.10/"),
            RtspSubUri: null,
            Credentials: new CameraCredentials(user, pass));

    [Fact]
    public async Task Add_StoresCameraAndCredentialsViaRefs()
    {
        var (service, _, secrets) = Create();

        var id = await service.AddAsync(BuildRequest(), CancellationToken.None);
        var stored = await service.GetAsync(id, CancellationToken.None);

        Assert.NotNull(stored);
        Assert.Equal("Front door", stored!.Name);

        // Credentials live in the secret store under predictable keys, never on the entity.
        Assert.Equal($"cam:{id}:username", stored.UsernameRef);
        Assert.Equal($"cam:{id}:password", stored.PasswordRef);
        Assert.Equal("admin", secrets.Items[stored.UsernameRef!]);
        Assert.Equal("12345", secrets.Items[stored.PasswordRef!]);
    }

    [Fact]
    public async Task GetCredentials_RoundTrips()
    {
        var (service, _, _) = Create();
        var id = await service.AddAsync(BuildRequest(user: "root", pass: "hunter2"), CancellationToken.None);

        var creds = await service.GetCredentialsAsync(id, CancellationToken.None);

        Assert.NotNull(creds);
        Assert.Equal("root", creds!.Username);
        Assert.Equal("hunter2", creds.Password);
    }

    [Fact]
    public async Task Update_ReplacesCredentialsWhenProvided()
    {
        var (service, _, secrets) = Create();
        var id = await service.AddAsync(BuildRequest(), CancellationToken.None);

        await service.UpdateAsync(id, new UpdateCameraRequest(
            Name: "Front porch",
            Host: "192.168.1.11",
            HttpPort: 80,
            OnvifPort: null,
            RtspMainUri: new Uri("rtsp://192.168.1.11/"),
            RtspSubUri: null,
            Credentials: new CameraCredentials("admin", "newpass")), CancellationToken.None);

        var creds = await service.GetCredentialsAsync(id, CancellationToken.None);
        Assert.Equal("newpass", creds!.Password);
        Assert.Equal("newpass", secrets.Items[$"cam:{id}:password"]);
    }

    [Fact]
    public async Task Update_LeavesCredentialsAloneWhenNotProvided()
    {
        var (service, _, _) = Create();
        var id = await service.AddAsync(BuildRequest(), CancellationToken.None);

        await service.UpdateAsync(id, new UpdateCameraRequest(
            Name: "Front porch",
            Host: "192.168.1.10",
            HttpPort: 80,
            OnvifPort: null,
            RtspMainUri: new Uri("rtsp://192.168.1.10/"),
            RtspSubUri: null,
            Credentials: null), CancellationToken.None);

        var creds = await service.GetCredentialsAsync(id, CancellationToken.None);
        Assert.Equal("12345", creds!.Password);
    }

    [Fact]
    public async Task Remove_VacuumsCredentials()
    {
        var (service, _, secrets) = Create();
        var id = await service.AddAsync(BuildRequest(), CancellationToken.None);

        await service.RemoveAsync(id, CancellationToken.None);

        Assert.Null(await service.GetAsync(id, CancellationToken.None));
        Assert.False(secrets.Items.ContainsKey($"cam:{id}:username"));
        Assert.False(secrets.Items.ContainsKey($"cam:{id}:password"));
    }
}
