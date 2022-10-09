#!/usr/bin/env sh

dotnet publish -c release -r linux-x64
cp bin/release/net7.0/linux-x64/publish/atlas-comments /srv/gemini/her.st/cgi/
rsync -vr bin/release/net7.0/linux-x64/publish/atlas-comments fortress.her.st:/srv/gemini/her.st/cgi/

# dotnet publish -c release -r linux-musl-x64
# rsync -vr bin/release/net7.0/linux-musl-x64/publish/atlas-comments fortress.her.st:/srv/gemini/her.st/cgi/