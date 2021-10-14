IN=icons-svg
OUT=icons
RES=64x64

echo "Cleaning OUT"
rm $OUT/*

for f in $IN/*
do
	echo "Converting $f"
	gtk-encode-symbolic-svg $f $RES -o $OUT
done

echo "Done!"
read
