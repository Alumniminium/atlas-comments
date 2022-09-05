#!/usr/bin/env sh

dotnet publish -c release -r linux-musl-x64
cp bin/release/net7.0/linux-musl-x64/publish/gemini-comments /srv/gemini/her.st/cgi/
rsync -r /srv/gemini/her.st fortress.her.st:/srv/gemini
