﻿using Core.Classes.Core;
using Core.Flags;
using Core.Serialization;
using Core.Serialization.Abstraction;
using Core.Types;
using Core.Types.PackageTables;

namespace Core.Utility;

public class PackageExporter
{
    private readonly FileSummary _exportHeader;
    private readonly Stream _exportStream;
    private readonly IStreamSerializer<ExportTableItem> _exporttTableItemSerializer;
    private readonly IStreamSerializer<FileSummary> _fileSummarySerializer;
    private readonly IStreamSerializer<ImportTableItem> _importTableItemSerializer;
    private readonly IStreamSerializer<FName> _nameSerializer;
    private readonly IStreamSerializer<NameTableItem> _nameTableItemSerializer;
    private readonly IStreamSerializer<ObjectIndex> _objectIndexSerializer;
    private readonly IUnrealPackageStream _outputPackageStream;
    private readonly UnrealPackage _package;

    public PackageExporter(UnrealPackage package, Stream exportStream, IStreamSerializer<FileSummary> fileSummarySerializer,
        IStreamSerializer<NameTableItem> nameTableItemSerializer, IStreamSerializer<ImportTableItem> importTableItemSerializer,
        IStreamSerializer<ExportTableItem> exporttTableItemSerializer, IStreamSerializer<ObjectIndex> objectIndexSerializer,
        IStreamSerializer<FName> nameSerializer)
    {
        _fileSummarySerializer = fileSummarySerializer;
        _nameTableItemSerializer = nameTableItemSerializer;
        _importTableItemSerializer = importTableItemSerializer;
        _exporttTableItemSerializer = exporttTableItemSerializer;
        _objectIndexSerializer = objectIndexSerializer;
        _nameSerializer = nameSerializer;
        _exportStream = exportStream;
        _package = package;
        _outputPackageStream = new UnrealPackageStream(_exportStream, _objectIndexSerializer, _nameSerializer, _package);
        _exportHeader = CopyHeader(_package.Header);
        ModifyHeaderFieldsForExport(_exportHeader);
    }

    private void ModifyHeaderFieldsForExport(FileSummary exportHeader)
    {
        exportHeader.LicenseeVersion = 0;
        exportHeader.ThumbnailTableOffset = 0;
        exportHeader.CookerVersion = 0;
        exportHeader.EngineVersion = 12791;

        var flags = (PackageFlags) exportHeader.PackageFlags;
        flags &= ~PackageFlags.PKG_Cooked;
        flags &= ~PackageFlags.PKG_StoreCompressed;

        //var a = flags.HasFlag(PackageFlags.PKG_AllowDownload);
        //var a1 = flags.HasFlag(PackageFlags.PKG_ClientOptional);
        //var a2 = flags.HasFlag(PackageFlags.PKG_ServerSideOnly);
        //var a3 = flags.HasFlag(PackageFlags.PKG_Cooked);
        //var a4 = flags.HasFlag(PackageFlags.PKG_Unsecure);
        //var a5 = flags.HasFlag(PackageFlags.PKG_SavedWithNewerVersion);
        //var a6 = flags.HasFlag(PackageFlags.PKG_Need);
        //var a7 = flags.HasFlag(PackageFlags.PKG_Compiling);
        //var a8 = flags.HasFlag(PackageFlags.PKG_ContainsMap);
        //var a9 = flags.HasFlag(PackageFlags.PKG_Trash);
        //var a10 = flags.HasFlag(PackageFlags.PKG_DisallowLazyLoading);
        //var a11 = flags.HasFlag(PackageFlags.PKG_PlayInEditor);
        //var a12 = flags.HasFlag(PackageFlags.PKG_ContainsScript);
        //var a13 = flags.HasFlag(PackageFlags.PKG_ContainsDebugInfo);
        //var a14 = flags.HasFlag(PackageFlags.PKG_RequireImportsAlreadyLoaded);
        //var a15 = flags.HasFlag(PackageFlags.PKG_SelfContainedLighting);
        //var a16 = flags.HasFlag(PackageFlags.PKG_StoreCompressed);
        //var a17 = flags.HasFlag(PackageFlags.PKG_StoreFullyCompressed);
        //var a18 = flags.HasFlag(PackageFlags.PKG_ContainsInlinedShaders);
        //var a19 = flags.HasFlag(PackageFlags.PKG_ContainsFaceFXData);
        //var a20 = flags.HasFlag(PackageFlags.PKG_NoExportAllowed);
        //var a21 = flags.HasFlag(PackageFlags.PKG_StrippedSource);
        exportHeader.PackageFlags = (uint) flags;
    }

    private FileSummary CopyHeader(FileSummary header)
    {
        var stream = new MemoryStream();
        _fileSummarySerializer.Serialize(stream, header);
        stream.Position = 0;
        return _fileSummarySerializer.Deserialize(stream);
    }

    /// <summary>
    ///     Write the complete package to the output stream
    /// </summary>
    public void ExportPackage()
    {
        ExportHeader();
        _exportHeader.NameOffset = (int) _exportStream.Position;
        ExportNameTable();
        _exportHeader.ImportOffset = (int) _exportStream.Position;
        ExportImportTable();
        _exportHeader.ExportOffset = (int) _exportStream.Position;
        ExporExporttTable();
        _exportHeader.DependsOffset = (int) _exportStream.Position;
        ExportDummyDependsTable();
        _exportHeader.ThumbnailTableOffset = 0;
        ExportDummyThumbnailsTable();
        ExportObjectSerialData();
        // re-export export table once all the exported data is known
        _exportStream.Position = _exportHeader.ExportOffset;
        ExporExporttTable();
        // re-export the header once all the header data is known
        ExportHeader();
    }

    /// <summary>
    ///     Writes the package header information to the start of the export stream
    /// </summary>
    public void ExportHeader()
    {
        _exportStream.Position = 0;
        _fileSummarySerializer.Serialize(_exportStream, _exportHeader);
    }

    /// <summary>
    ///     Writes the name table to the current position of the stream. Will not verify the stream offset to be the same as
    ///     <see cref="FileSummary.NameOffset" />
    /// </summary>
    public void ExportNameTable()
    {
        _nameTableItemSerializer.WriteTArray(_exportStream, _package.NameTable.ToArray(), StreamSerializerForExtension.ArraySizeSerialization.NoSize);
    }

    /// <summary>
    ///     Writes the import table to the current position of the stream. Will not verify the stream offset to be the same as
    ///     <see cref="FileSummary.ImportOffset" />
    /// </summary>
    public void ExportImportTable()
    {
        var importObjects = _package.ImportTable.Select(x => x.ImportedObject).ToList();
        //var newImportTable = new ImportTable();
        _importTableItemSerializer.WriteTArray(_exportStream, _package.ImportTable.ToArray(), StreamSerializerForExtension.ArraySizeSerialization.NoSize);
    }

    /// <summary>
    ///     Writes the export table to the current position of the stream. Will not verify the stream offset to be the same as
    ///     <see cref="FileSummary.ExportOffset" />
    /// </summary>
    public void ExporExporttTable()
    {
        _exporttTableItemSerializer.WriteTArray(_exportStream, _package.ExportTable.ToArray(), StreamSerializerForExtension.ArraySizeSerialization.NoSize);
    }

    /// <summary>
    ///     Write a empty depends table to the stream. Will not verify the stream offset to be the same as
    ///     <see cref="FileSummary.DependsOffset" />
    /// </summary>
    public void ExportDummyDependsTable()
    {
        for (var i = 0; i < _package.Header.ImportCount; i++)
        {
            _exportStream.WriteInt32(0);
        }
    }

    /// <summary>
    ///     Does nothing as this exporter is made to skip thumbnails all together. The thumbnail data would be serialized after
    ///     the export table. after the thumbnail data the thumbnail table would be serialized.
    ///     The reason for serializing the table after data, is that the table requires the data offsets to be known
    /// </summary>
    public void ExportDummyThumbnailsTable()
    {
    }

    /// <summary>
    ///     Write the object serial data to the output stream. Will throw if any objects serializers fails to resolve.
    ///     Overwrite the export table entries of the object package with new offsets and sizes.
    /// </summary>
    public void ExportObjectSerialData(IObjectSerializerFactory? objectSerializerFactory = null)
    {
        var exports = _package.ExportTable;
        foreach (var export in exports)
        {
            var obj = export.Object;
            if (!obj.FullyDeserialized)
            {
                obj.Deserialize();
            }

            var offset = _exportStream.Position;
            var serializer = GetObjectSerializer(obj, objectSerializerFactory);
            serializer.SerializeObject(obj, _outputPackageStream);
            var size = _exportStream.Position - offset;
            export.SerialOffset = offset;
            export.SerialSize = (int) size;
        }
    }

    private static IObjectSerializer GetObjectSerializer(UObject obj, IObjectSerializerFactory? factory)
    {
        if (factory != null)
        {
            return factory.GetSerializer(obj.GetType()) ?? throw new InvalidOperationException();
        }

        ArgumentNullException.ThrowIfNull(obj?.Serializer);
        return obj.Serializer;
    }
}