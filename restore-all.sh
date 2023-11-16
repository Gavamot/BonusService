#!/bin/bash
for projectName in *.csproj; do
	dotnet restore $projectName
done