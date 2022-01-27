var emojiDefinitions = {
	exports: {}
};
webpackChunkdiscord_app.find(a => a[1][503033])[1][503033](emojiDefinitions);

var emojiSvgTableGenerator = {
	exports: {}
};
webpackChunkdiscord_app.find(a => a[1][553895])[1][553895](emojiSvgTableGenerator, undefined, { o: function (a, b) { return true; } });

function stringToEntityList(srcString, optionalJoiner) {
	srcString = srcString.indexOf("\u200D") < 0 ? srcString.replace("\uFE0F", "") : srcString;
	for (var n = [], r = 0, i = 0, a = 0; a < srcString.length;) {
		r = srcString.charCodeAt(a++);
		if (i) {
			n.push((65536 + (i - 55296 << 10) + (r - 56320)).toString(16));
			i = 0
		} else 55296 <= r && r <= 56319 ? i = r : n.push(r.toString(16))
	}
	return n.join(optionalJoiner || "-")
}

var emojiFilenameTable = {};
var errors = 0;
Object.entries(emojiDefinitions.exports).forEach((k) => {
	var rootObj = { p: "" };
	k[1].forEach((emoji) => {
		allEmojis.push(emoji.surrogates);

		var surrogates = stringToEntityList(emoji.surrogates);
		var srcFilename = "./" + surrogates + ".svg";
		var filenameFuncId = emojiSvgTableGenerator.exports.resolve(srcFilename);
		var filenameFuncContainer = webpackChunkdiscord_app.find(a => a[1][filenameFuncId]);

		if (filenameFuncContainer === undefined) { 
			console.log("Surrogate", emoji.surrogates, "produced", surrogates, "-- func", filenameFuncId, "-- which does not have an SVG");
			errors++;
			return;
		}

		var filenameFunc = filenameFuncContainer[1][filenameFuncId];

		var intermediary = { exports: {} };
		filenameFunc(intermediary, undefined, rootObj);
		var filename = intermediary.exports;

		emojiFilenameTable[surrogates] = filename;
	});
});

console.log("Encountered", errors, "errors");

function downloadJson(filename, data) {
	var data = JSON.stringify(data, undefined, 4)

	var blob = new Blob([data], { type: 'text/json' }),
		e = document.createEvent('MouseEvents'),
		a = document.createElement('a')

	a.download = filename;
	a.href = window.URL.createObjectURL(blob);
	a.dataset.downloadurl = ['text/json', a.download, a.href].join(':');
	e.initMouseEvent('click', true, false, window, 0, 0, 0, 0, 0, false, false, false, false, 0, null);
	a.dispatchEvent(e);
}

downloadJson("emoji_surrogate_to_filename_table.json", emojiFilenameTable);