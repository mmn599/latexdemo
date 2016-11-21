cd C:\\Users\\mmnor\\Projects\\LatexDemo\\LatexDemo
pdflatex -jobname=output C:\\Users\\mmnor\\Projects\\LatexDemo\\LatexDemo\\mylatex
cd "C:\Program Files\ImageMagick-7.0.3-Q16\"
convert.exe -density 500 -alpha on C:\\Users\\mmnor\\Projects\\LatexDemo\\LatexDemo\\output.pdf C:\\Users\\mmnor\\Projects\\LatexDemo\\LatexDemo\\output.png"