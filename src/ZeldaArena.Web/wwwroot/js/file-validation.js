// Клиентская проверка загружаемого изображения для jquery-validation-unobtrusive.
(function ($) {
  'use strict';

  if (!$ || !$.validator || !$.validator.unobtrusive) {
    return;
  }

  $.validator.addMethod('imagefile', function (value, element, params) {
    if (!element.files || element.files.length === 0) {
      return true;
    }

    var file = element.files[0];
    var dot = file.name.lastIndexOf('.');
    var extension = dot < 0 ? '' : file.name.slice(dot).toLowerCase();

    return params.extensions.split(',').indexOf(extension) >= 0
      && file.size > 0
      && file.size <= Number(params.maxsize);
  });

  $.validator.unobtrusive.adapters.add('imagefile', ['extensions', 'maxsize'], function (options) {
    options.rules.imagefile = { extensions: options.params.extensions, maxsize: options.params.maxsize };
    options.messages.imagefile = options.message;
  });
})(window.jQuery);