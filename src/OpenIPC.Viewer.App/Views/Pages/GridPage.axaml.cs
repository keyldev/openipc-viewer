using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Messaging;
using OpenIPC.Viewer.App.Messages;
using OpenIPC.Viewer.App.ViewModels;

namespace OpenIPC.Viewer.App.Views.Pages;

public sealed partial class GridPage : UserControl
{
    // In-process custom format: payload is the source tile's index in Tiles
    // as a decimal string. Tile reorder never crosses process boundaries, so
    // InProcessFormat avoids any clipboard/serializer plumbing.
    private static readonly DataFormat<string> TileIndexFormat =
        DataFormat.CreateStringApplicationFormat("openipc-viewer/grid-tile-index");

    public GridPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is GridPageViewModel vm)
                await vm.LoadAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[GridPage] OnLoaded failed: {ex}");
        }
    }

    private void OnTileTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Control { DataContext: CameraTileViewModel tile })
            WeakReferenceMessenger.Default.Send(new OpenCameraMessage(tile.Camera.Id));
    }

    // Drag start. Source is the inner tile Grid (DataContext is the tile VM);
    // we look up its index in Tiles via the parent ItemsControl so VM-driven
    // reorders stay the single source of truth for ordering.
    private async void OnTilePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control source) return;
        if (source.DataContext is not CameraTileViewModel tile) return;
        if (DataContext is not GridPageViewModel vm) return;
        if (!e.GetCurrentPoint(source).Properties.IsLeftButtonPressed) return;

        var index = vm.Tiles.IndexOf(tile);
        if (index < 0) return;

        try
        {
            var transfer = new DataTransfer();
            transfer.Add(DataTransferItem.Create(TileIndexFormat, index.ToString(CultureInfo.InvariantCulture)));
            await DragDrop.DoDragDropAsync(e, transfer, DragDropEffects.Move);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[GridPage] drag start failed: {ex}");
        }
    }

    // Each slot Border gets DragOver/Drop attached once it enters the tree.
    // Using AttachedToVisualTree keeps the handlers paired with the visual
    // (re-templated items get fresh wiring) without a global Topmost handler.
    private void OnSlotAttached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is not Control slot) return;
        DragDrop.AddDragOverHandler(slot, OnSlotDragOver);
        DragDrop.AddDropHandler(slot, OnSlotDrop);
    }

    private void OnSlotDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.DataTransfer.Contains(TileIndexFormat)
            ? DragDropEffects.Move
            : DragDropEffects.None;
        e.Handled = true;
    }

    private async void OnSlotDrop(object? sender, DragEventArgs e)
    {
        try
        {
            if (sender is not Control slot) return;
            if (DataContext is not GridPageViewModel vm) return;

            // Empty placeholder slots have a null DataContext — dropping there
            // is a no-op (no underlying camera to swap with).
            if (slot.DataContext is not CameraTileViewModel target) return;

            var raw = e.DataTransfer.TryGetValue(TileIndexFormat);
            if (raw is null || !int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var from))
                return;

            var to = vm.Tiles.IndexOf(target);
            if (to < 0) return;

            await vm.MoveTileAsync(from, to, CancellationToken.None);
            e.Handled = true;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[GridPage] drop failed: {ex}");
        }
    }
}
