@echo off
echo Running Financeiro Tests...
cd /d "C:\dev\carnacode-2026\balta-desafio-carnacode-2026_19-observer\src\Financeiro\Financeiro.Tests"
dotnet test --logger "console;verbosity=normal" --no-restore
pause

