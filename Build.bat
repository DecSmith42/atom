@echo off

:: Variables
  setlocal
  
:: Workflow
  :: PackAtom
  echo Running PackAtom...
  dotnet run --project _atom\_atom.csproj PackAtom --skip
  
:: Cleanup
  endlocal
