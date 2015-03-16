"use strict";

$(document).ready(function () {
    $('body').scrollspy();

    // 複数のフォームがある場合のための処理
    var form = $('form')[0];
    if (form) {
        // フォームでEnterキーの入力を禁止する
        // ページ内でEnterを押したときページの一番上のsubmitボタンがクリックされた扱いになってしまうのを防ぐ
        form.onkeypress = function (e) {
            // textareaではEnterを許可
            if (!(e.srcElement instanceof HTMLTextAreaElement) && event.keyCode == 13) {
                return false;
            }
        }

        $.each($('input'), function (idx, elem) {
            if (elem.type != "submit" && elem.getAttribute("submitButton")) {
                // 入力欄にsubmitButton属性がついている場合
                // Enterが押されたときsubmitButtonに書かれたIDのボタンをクリックさせる
                elem.onkeypress = function (event) {
                    var elem = event.target;
                    if (event.keyCode == 13) {
                        $(document.getElementById(elem.getAttribute("submitButton"))).click();
                        return false;
                    }
                }
            }
        });
    }
});

