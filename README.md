## ZPL Generator

Generating ZPL by hand is tedious. The `ZplGenerator` class uses the builder pattern to make it less painful.

The included console app demonstrates how to use the `ZplGenerator` class to build ZPL. After running, the app automatically copies the generated ZPL to the clipboard.

When the ZPL code is sent to a Zebra printer, it will render as a label. To preview what the printed output will look like, go [here](https://labelary.com/viewer.html) and paste the ZPL. This viewer doesn't always render the same as the printer, but in most cases it is very close. The printer is ultimately the source of truth.

<br>

**NOTE:**  *The `suppressCrLf` parameter has been set to `True` when instantiating the `ZplGenerator` class. This makes the generated ZPL more readable, but should be removed for production code.*




