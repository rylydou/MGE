#!/bin/bash

FEATURES=tnum,ss03,aa04,cv10

pyftfeatfreeze -f $FEATURES Resources/Inter-Medium.otf Regular.ttf &
pyftfeatfreeze -f $FEATURES Resources/Inter-MediumItalic.otf Regular\ Italic.ttf &

pyftfeatfreeze -f $FEATURES Resources/Inter-Bold.otf Bold.ttf &
pyftfeatfreeze -f $FEATURES Resources/Inter-BoldItalic.otf Bold\ Italic.ttf
