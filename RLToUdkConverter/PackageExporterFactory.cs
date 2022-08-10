﻿using Core.Serialization;
using Core.Types;
using Core.Types.PackageTables;
using Core.Utility.Export;

namespace RLToUdkConverter;

public class PackageExporterFactory
{
    private readonly IStreamSerializer<ExportTableItem> _exportTableItemSerializer;

    private readonly IStreamSerializer<FileSummary> _filesummarySerializer;

    private readonly IStreamSerializer<ImportTableItem> _importTableItemSerializer;

    private readonly IStreamSerializer<FName> _nameSerializer;

    private readonly IStreamSerializer<NameTableItem> _nameTableItemSerializer;

    private readonly IStreamSerializer<ObjectIndex> _objectIndexSerializer;

    public PackageExporterFactory(IStreamSerializer<FName> nameSerializer, IStreamSerializer<ObjectIndex> objectIndexSerializer,
        IStreamSerializer<ExportTableItem> exportTableItemSerializer, IStreamSerializer<ImportTableItem> importTableItemSerializer,
        IStreamSerializer<NameTableItem> nameTableItemSerializer, IStreamSerializer<FileSummary> filesummarySerializer)
    {
        _nameSerializer = nameSerializer;
        _objectIndexSerializer = objectIndexSerializer;
        _exportTableItemSerializer = exportTableItemSerializer;
        _importTableItemSerializer = importTableItemSerializer;
        _nameTableItemSerializer = nameTableItemSerializer;
        _filesummarySerializer = filesummarySerializer;
    }

    public PackageExporter Create(UnrealPackage package, Stream outputStream)
    {
        return new PackageExporter(package, outputStream, _filesummarySerializer, _nameTableItemSerializer, _importTableItemSerializer,
            _exportTableItemSerializer, _objectIndexSerializer, _nameSerializer);
    }
}