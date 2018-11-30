// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

/**
 * Number.prototype.format(n, x)
 * 
 * @param n: length of decimal
 * @param x: length of sections
 */
// ReSharper disable once NativeTypePrototypeExtending
Number.prototype.format = function (n, x) {
    const re = `\\d(?=(\\d{${x || 3}})+${n > 0 ? "\\." : "$"})`;
    return this.toFixed(Math.max(0, ~~n)).replace(new RegExp(re, "g"), "$&,");
};
