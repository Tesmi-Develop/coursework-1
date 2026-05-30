namespace Coursework1;

public interface IFileDataHandler
{
    bool TryExport(string filePath, out string error);
    bool TryImport(string filePath, out string error);
}