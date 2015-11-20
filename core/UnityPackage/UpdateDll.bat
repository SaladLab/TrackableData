pushd %~dp0

SET SRC=..\TrackableData.Net35\bin\Release
SET SRC_JSON=..\..\plugins\TrackableData.Json.Net35\bin\Release
SET SRC_PROTOBUF=..\..\plugins\TrackableData.Protobuf.Net35\bin\Release
SET DST=.\Assets\Middlewares\TrackableData
SET PDB2MDB=..\..\tools\unity3d\pdb2mdb.exe

%PDB2MDB% "%SRC%\TrackableData.dll"
%PDB2MDB% "%SRC_JSON%\TrackableData.Json.dll"
%PDB2MDB% "%SRC_PROTOBUF%\TrackableData.Protobuf.dll"

COPY /Y "%SRC%\TrackableData.dll*" %DST%
COPY /Y "%SRC_JSON%\TrackableData.Json.dll*" %DST%
COPY /Y "%SRC_PROTOBUF%\TrackableData.Protobuf.dll*" %DST%

popd
