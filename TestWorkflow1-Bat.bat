@echo off

:: Variables
    setlocal
    
:: Workflow
    :: PackAtom
    echo Running PackAtom...
    dotnet run --project DecSm.Atom.Sample\DecSm.Atom.Sample.csproj PackAtom --skip
    
:: Cleanup
    endlocal
