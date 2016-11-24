cd C:\\Users\\mmnor\\Projects\\LatexDemo\\LatexDemo\\temp
pdflatex -jobname=output -interaction=batchmode %2
cd "C:\Program Files\ImageMagick-7.0.3-Q16\"
convert.exe -density 500 -alpha on %3 %1