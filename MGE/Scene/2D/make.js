const fs = require('fs')

let str = 'using System;\n\nnamespace MGE;\n\n[Flags]\npublic enum CollisionLayer : uint\n{\n'

for (let i = 0; i < 32; i++) {
	if (i == 16) str += '\n'
	str += `\tLayer${i + 1} = ${Math.pow(2, i)},\n`
}

str += '}\n'

fs.writeFileSync('./CollisionLayer.cs', str)

console.log("Done")
