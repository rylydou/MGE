---
uid: index.md
title: What is meml
---

# Introduction

Meml stands for **M**angrove **E**ngine **M**arkup **L**angauge. It is the language used for storing data in a readable format in Mangrove.

Meml is based off of [Hjson](https://hjson.github.io/), a more human readable version of json.

Meml supports objects...

```meml
{
	key: 'value'

	myNumber: 123
}
```

And arrays

```meml
myArray: [ 2 4 6 8 ]

myOtherArray: [
	'How'
	'are'
	'you'
	'doing'
	'today?'
]
```

# Types

Meml also has support for many of the primitive types in C#

```meml
# Oh yeah and these are comments too!
string: 'Hello world'

nullValue: null

# Integers
int: 123
alternativeInt: 123i
uint: 123I
long: 123l
ulong: 123L
short: 123s
ushort: 123S
byte: 123B
sbyte: 123b
char: 123c

# Floating points
single: 123.456
double: 123.456d
decimal: 123.456m
notANumber: nan
infinity: inf
```

Plus byte arrays to store binary data.

```meml
data: *SGVsbG8gd29ybGQ=* # Encoded as base64
```

# Object

Types of objects are defined like this

```meml
{
	!Assembly: 'Namespace.Type'
	key: 'value'
}
```

# Arrays

```meml
myArray: [ 2 4 6 8 ]

myOtherArray: [
	'How'
	'are'
	'you'
	'doing'
	'today?'
]
```

Meml does **not** support tables, instead do something like this if you need to create a table

```meml
table: [
	{
		key: 42
		value 'meaning of life'
	}
]
```

# Variables

All variables must be defining at the start of the file (ignoring comments), they must start with a '$' sign.

```meml
# Variable is defined here
$theNumber: 42

$robot: {
	meaningOfLife: $theNumber
}

myRobot: $robot
```

# Notes
