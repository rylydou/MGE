#!/bin/bash

INTER=tnum,ss03,ss04,cv10

pyftfeatfreeze -f $INTER Fonts/Inter/Resources/Inter-Regular.otf Fonts/Inter/Regular.ttf
pyftfeatfreeze -f $INTER Fonts/Inter/Resources/Inter-Italic.otf Fonts/Inter/Regular\ Italic.ttf

pyftfeatfreeze -f $INTER Fonts/Inter/Resources/Inter-Bold.otf Fonts/Inter/Bold.ttf
pyftfeatfreeze -f $INTER Fonts/Inter/Resources/Inter-BoldItalic.otf Fonts/Inter/Bold\ Italic.ttf
