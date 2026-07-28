ISCC := C:/Program Files (x86)/Inno Setup 6/ISCC.exe

PUBLISH_FLAGS := -c Release -r win-x64 --no-self-contained -p:PublishSingleFile=true -p:DebugType=none --nologo

SVC_EXE := NvidiaSmiProxy/bin/x64/Debug/net10.0-windows/NvidiaSmiProxy.exe
APP_EXE := VRCGPUTool/bin/x64/Debug/net10.0-windows/VRChatGPUTool.exe

.PHONY: all build test coverage publish installer run clean

all: installer

build:
	dotnet build VRCGPUTool.slnx --nologo

test:
	dotnet test VRCGPUTool.Tests/VRCGPUTool.Tests.csproj --nologo

coverage:
	rm -rf coverage
	dotnet test --settings coverage.runsettings --collect:"XPlat Code Coverage" --results-directory ./coverage
	reportgenerator -reports:"coverage/**/coverage.cobertura.xml" -targetdir:"coverage/report" -reporttypes:"Html;TextSummary"
	cmd //c start "" "coverage\\report\\index.html"

run: build
	powershell -NoProfile -Command "Start-Process '$(SVC_EXE)'; Start-Process '$(APP_EXE)'"

publish:
	dotnet publish VRCGPUTool/VRCGPUTool.csproj \
	    $(PUBLISH_FLAGS) --output publish/app/
	dotnet publish NvidiaSmiProxy/NvidiaSmiProxy.csproj \
	    $(PUBLISH_FLAGS) --output publish/service/

installer: publish
	"$(ISCC)" setup.iss

clean:
	dotnet clean VRCGPUTool.slnx --nologo
	rm -rf dist publish
