#!/bin/sh
set -e
mono .paket/paket.bootstrapper.exe

# mozroots --import --sync --quiet

mono .paket/paket.exe restore -f
mono packages/FAKE/tools/FAKE.exe build.fsx compile