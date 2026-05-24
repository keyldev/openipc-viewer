using System.Data.Common;

namespace OpenIPC.Viewer.Infrastructure.Persistence;

public interface IDbConnectionFactory
{
    DbConnection OpenConnection();
}
