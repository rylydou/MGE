IN=icons-svg
OUT=icons
RES=16x16 # Maybe try 32x32 or 64x64?

echo "Cleaning OUT"
rm $OUT\*

for f in $IN\*
do
	echo "Converting $f"
	gtk-encode-symbolic-svg $f $RES -o icons-symbolic
done

read