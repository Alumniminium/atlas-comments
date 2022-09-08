#!/usr/bin/env sh

dotnet publish -c release -r linux-musl-x64
cp bin/release/net7.0/linux-musl-x64/publish/atlas-comments bin/release/net7.0/linux-musl-x64/publish/atlas-comments.pdb /srv/gemini/her.st/cgi/
rsync -vr /srv/gemini/her.st fortress.her.st:/srv/gemini
