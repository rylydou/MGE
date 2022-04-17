const path = require('path')
const fs = require('fs')

exports.findFilesRecursive = (startPath, filter, callback) => {
	let files = fs.readdirSync(startPath)

	for (let i = 0; i < files.length; i++) {
		let filename = path.join(startPath, files[i].replace(/\\/g, '/'))
		let stat = fs.lstatSync(filename)

		if (stat.isDirectory()) {
			exports.findFilesRecursive(filename, filter, callback) // Recurse
		}
		else if (filter.test(filename)) callback(filename)
	};
}

exports.copyFileSync = (source, target) => {
	let targetFile = target

	// If target is a directory, a new file with the same name will be created
	if (fs.existsSync(target)) {
		if (fs.lstatSync(target).isDirectory()) {
			targetFile = path.join(target, path.basename(source))
		}
	}

	fs.writeFileSync(targetFile, fs.readFileSync(source))
}

exports.copyFolderRecursiveSync = (source, target) => {
	let files = []

	// Check if folder needs to be created or integrated
	let targetFolder = path.join(target, path.basename(source))
	if (!fs.existsSync(targetFolder)) {
		fs.mkdirSync(targetFolder)
	}

	// Copy
	if (fs.lstatSync(source).isDirectory()) {
		files = fs.readdirSync(source)
		files.forEach(function (file) {
			let curSource = path.join(source, file)
			if (fs.lstatSync(curSource).isDirectory()) {
				exports.copyFolderRecursiveSync(curSource, targetFolder)
			} else {
				exports.copyFileSync(curSource, targetFolder)
			}
		})
	}
}

exports.convertPath = (path) => {
	// Remove `.md`, will get changed to `.html`
	path = path.replace(/\.md$/, '')
	// Remove the number and space at the beginning of the file and folders used for ordering
	path = path.replace(/\d+_/g, '')
	// Remove `/index`, special case to handle index files
	path = path.replace(/\/?index$/, '')

	return path
}
