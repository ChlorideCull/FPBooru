#!/bin/sh
# Requires ImageMagick and GNU coreutils
echo "Building thumbnails from sample images"
mogrify -path build/static/thumbs/ -thumbnail 648x324^ -gravity center -extent 648x324 sampleimgs/*
echo "Building headers from sample images"
mogrify -path build/static/headers/ -thumbnail 1920x100^ -gravity center -extent 1920x100 sampleimgs/*
echo "Done!"