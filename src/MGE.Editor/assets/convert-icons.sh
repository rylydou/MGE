IN=icons-svg
OUT=icons
RES=64x64

echo "Checking for encoder"
command -v gtk-encode-symbolic-svg || {
	echo "GTK tools are not installed."
	exit 1
}

echo "Cleaning OUT"
rm $OUT/*

echo "Converting svgs"
for file in $IN/*
do
	echo "Converting $file"
	gtk-encode-symbolic-svg $file $RES -o $OUT
done

echo "Done!"
exit 0
