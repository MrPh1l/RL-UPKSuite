﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Classes.Core;
using Core.Types;
using PackageExplorer.Model;
using Syncfusion.UI.Xaml.TreeView.Engine;

namespace PackageExplorer.ViewModel;

public partial class PackageUserControlViewModel : ObservableObject
{
    private readonly UnrealPackage _package;

    private readonly Dictionary<UObject, List<UObject>> _packageGraph = new();

    [ObservableProperty]
    private PackageObject? _selectedNode;

    [ObservableProperty]
    private bool _showDefaultObjects;

    public PackageUserControlViewModel(UnrealPackage package)
    {
        _package = package;

        List<UObject> rootObjects = new();
        foreach (var export in _package.ExportTable)
        {
            var exportObj = export.Object;
            if (exportObj == null)
            {
                continue;
            }

            var outer = exportObj.Outer;
            if (outer is null || outer == package.PackageRoot)
            {
                rootObjects.Add(exportObj);
                continue;
            }

            if (!_packageGraph.ContainsKey(outer))
            {
                _packageGraph.Add(outer, new List<UObject>());
            }

            _packageGraph[outer].Add(exportObj);
        }

        foreach (var rootObject in rootObjects)
        {
            if (rootObject.Class?.Name == "Class")
            {
                continue;
            }

            var obj = new PackageObject(rootObject);
            var subObjects = _packageGraph.ContainsKey(rootObject) ? _packageGraph[rootObject] : new List<UObject>();
            AddTopLevelObject(obj, subObjects);
        }

        PackageView = new ListCollectionView(TopLevelObjects)
        {
            Filter = FilterPackageObjects
        };

        //foreach (var export in package.ExportTable.Where(x => x.OuterIndex.Index == 0))
        //{
        //    var exportedObject = export.Object;

        //    if (exportedObject is null || exportedObject.Class?.Name == "Class")
        //    {
        //        continue;
        //    }

        //    var obj = new PackageObject(exportedObject);
        //    AddTopLevelObject(obj);
        //}
    }

    public string PackageName => _package.PackageName;

    public ObservableCollection<PackageObject> TopLevelObjects { get; set; } = new();

    public ListCollectionView PackageView { get; set; }

    private bool FilterPackageObjects(object o)
    {
        if (_showDefaultObjects)
        {
            return true;
        }

        var obj = o as PackageObject;
        return !obj.IsDefaultObject;
    }


    [ICommand(CanExecute = nameof(CanExecuteOnDemandLoading))]
    private void ExecuteOnDemandLoading(TreeViewNode node)
    {
        if (node.ChildNodes.Count > 0)
        {
            node.IsExpanded = true;
            return;
        }

        if (node.Content is not PackageObject packageObject)
        {
            return;
        }


        if (packageObject.Children.Count == 0)
        {
            node.HasChildNodes = false;
            return;
        }

        foreach (var child in packageObject.Children)
        {
            PopulateChildren(child);
        }

        node.PopulateChildNodes(packageObject.Children);


        node.IsExpanded = true;
    }

    [ICommand]
    private void OnShowDefaultChanged(bool isChecked)
    {
        PackageView.Refresh();
    }

    public void AddTopLevelObject(PackageObject obj)
    {
        PopulateChildren(obj);
        TopLevelObjects.Add(obj);
    }

    public void AddTopLevelObject(PackageObject obj, List<UObject> subObjects)
    {
        foreach (var subObject in subObjects)
        {
            obj.Children.Add(new PackageObject(subObject));
        }

        TopLevelObjects.Add(obj);
    }

    private bool CanExecuteOnDemandLoading(TreeViewNode node)
    {
        return (node.Content as PackageObject)?.HasSubObjects ?? false;
    }

    private void PopulateChildren(PackageObject packageObject)
    {
        var subObjects = _packageGraph.ContainsKey(packageObject.Object) ? _packageGraph[packageObject.Object] : new List<UObject>();
        foreach (var subObject in subObjects)
        {
            packageObject.Children.Add(new PackageObject(subObject));
        }
        //foreach (var subObject in _package.ExportTable.Where(x => x.Object?.Outer == packageObject.Object))
        //{
        //    packageObject.Children.Add(new PackageObject(subObject.Object!));
        //}
    }
}