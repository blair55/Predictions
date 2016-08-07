# mono .paket/paket.bootstrapper.exe

mozroots --import --sync --quiet

mono .paket/paket.exe restore
mono packages/FAKE/tools/FAKE.exe build.fsx compile
