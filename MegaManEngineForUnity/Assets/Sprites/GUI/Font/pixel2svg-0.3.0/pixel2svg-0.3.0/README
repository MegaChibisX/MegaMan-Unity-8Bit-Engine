README for pixel2svg
====================

About
-----

pixel2svg converts pixel art to SVG - pixel by pixel.

For example, here is an icon from the [Tango Icon 
Set](http://tango.freedesktop.org/):

![tango heart](http://static.florian-berger.de/tango-heart.png)

If you scale this up for a nice blocky print, you might get a dithered result:

![tango heart 400px dithered](http://static.florian-berger.de/tango-heart-400px-dithered.png)

Of course you can turn dithering off. But sometimes you might want a vector 
file, especially for large prints. For these cases, pixel2svg produces this SVG 
file (try clicking to find out whether your browser supports SVG):

[tango-heart.svg](http://static.florian-berger.de/tango-heart.svg)

Here is a screenshot of the SVG in [Inkscape](http://inkscape.org/):

![tango heart inkscape](http://static.florian-berger.de/tango-heart-inkscape.png)

Nice, pure vector data.


Prerequisites
-------------

Python 2.x   [http://www.python.org](http://www.python.org)

Python Imaging Library (PIL) [http://www.pythonware.com/products/pil/](http://www.pythonware.com/products/pil/)

svgwrite [http://pypi.python.org/pypi/svgwrite/](http://pypi.python.org/pypi/svgwrite/)


Installation
------------

Unzip the file, then at the command line run

    python setup.py install


Usage
-----

    Usage: pixel2svg [--overlap] IMAGEFILE

    Options:
      --version             show program's version number and exit
      -h, --help            show this help message and exit
      --squaresize=SQUARESIZE
                            Width and heigt of vector squares in pixels, default: 40
      --overlap             If given, overlap vector squares by 1px

Running

    pixel2svg.py IMAGE.EXT

will process IMAGE.EXT and create IMAGE.svg.

EXT can be any format (png, jpg etc.) that can be read by the Python Imaging 
Library.


License
-------

pixel2svg is licensed under the GPL. See the file COPYING for details.


Author
------

(c) Florian Berger <fberger@florian-berger.de>
