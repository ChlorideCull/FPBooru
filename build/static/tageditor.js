/*
Copyright (c) 2015 Sebastian "Chloride Cull" Johansson

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
$(function () {
    var renderItems = function (elem) {
        $(elem).children(".tag-item").remove();
        var readonly = ($(elem.children["tags"]).attr("readonly") != undefined);

        $.each(elem.children["tags"].value.split(","), function (index, val) {
            if (val.trim() === "") return;
            $("<div class=\"tag-item\" />").append(
            $("<span />").text(val.trim())).append(
            readonly ? $("<a href=\"#\"></a>").text("(0)") : $("<a href=\"#\">X</a>").click(function (eObj) {
                var formval = elem.children["tags"].value;
                elem.children["tags"].value = formval.substr(0, formval.indexOf(val.trim())) + formval.substr(formval.indexOf(val.trim()) + val.trim().length + 2, formval.length);
                renderItems(elem);
            })).attr("style", (function (tagcont) {
                if (tagcont.substring(0, 8) == "creator:") return "box-shadow: 0px 3px #E96D01; background-color: #E86C00;";
                else return "box-shadow: 0px 3px #01BB2A; background-color: #00BA29;";
            })(val.trim())).appendTo($(elem));
        });
        if (!readonly) {
            $(elem).append($("<div class=\"tag-item tag-item-adder\">+</div>").click(function (eObj) {
                eObj.onclick = function () {}; //Prevent multiple instances
                eObj.target.innerHTML = "";
                $(eObj.target).append($("<form />").submit(function (eObj2) {
                    elem.children["tags"].value += eObj2.target[0].value + ", ";
                    eObj.target.innerText = "+";
                    renderItems(elem);
                    return false;
                }).append($("<input />").focusout(function () {
                    renderItems(elem);
                })));
                setTimeout(function () {
                    $(eObj.target).children("form")[0][0].focus();
                }, 0);
            }));
        }
    };

    $("div.tageditor").each(function (index, elem) {
        elem.children["tags"].setAttribute("style", "display: none;");
        renderItems(elem);
    });
});