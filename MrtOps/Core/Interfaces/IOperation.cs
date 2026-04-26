namespace MrtOps.Core.Interfaces;

public interface IOperation
{
    string Description { get; }
    bool Execute();
    bool Undo();
}