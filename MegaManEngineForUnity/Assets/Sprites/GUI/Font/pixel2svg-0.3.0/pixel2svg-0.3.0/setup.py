"""pixel2svg Setup Script

   Copyright  Florian Berger <fberger@florian-berger.de>
"""

# This file is part of pixel2svg.
#
# pixel2svg is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# pixel2svg is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with pixel2svg.  If not, see <http://www.gnu.org/licenses/>.

# Work started on Thu Jul 21 08:08:38 CEST 2011.

import distutils.core
import pixel2svg

LONG_DESCRIPTION = """About
-----

pixel2svg converts pixel art to SVG - pixel by pixel.

For example, here is an icon from the `Tango Icon
Set <http://tango.freedesktop.org/>`_:

.. figure:: http://static.florian-berger.de/tango-heart.png
   :align: center
   :alt: tango heart

   tango heart
If you scale this up for a nice blocky print, you might get a dithered
result:

.. figure:: http://static.florian-berger.de/tango-heart-400px-dithered.png
   :align: center
   :alt: tango heart 400px dithered

   tango heart 400px dithered
Of course you can turn dithering off. But sometimes you might want a
vector file, especially for large prints. For these cases, pixel2svg
produces this SVG file (try clicking to find out whether your browser
supports SVG):

`tango-heart.svg <http://static.florian-berger.de/tango-heart.svg>`_

Here is a screenshot of the SVG in `Inkscape <http://inkscape.org/>`_:

.. figure:: http://static.florian-berger.de/tango-heart-inkscape.png
   :align: center
   :alt: tango heart inkscape

   tango heart inkscape
Nice, pure vector data.

Prerequisites
-------------

Python 2.x `http://www.python.org <http://www.python.org>`_

Python Imaging Library (PIL)
`http://www.pythonware.com/products/pil/ <http://www.pythonware.com/products/pil/>`_

svgwrite
`http://pypi.python.org/pypi/svgwrite/ <http://pypi.python.org/pypi/svgwrite/>`_

Installation
------------

Unzip the file, then at the command line run

::

    python setup.py install

Usage
-----

::

    Usage: pixel2svg [--overlap] IMAGEFILE

    Options:
      --version             show program's version number and exit
      -h, --help            show this help message and exit
      --squaresize=SQUARESIZE
                            Width and heigt of vector squares in pixels, default: 40
      --overlap             If given, overlap vector squares by 1px

Running

::

    pixel2svg.py IMAGE.EXT

will process IMAGE.EXT and create IMAGE.svg.

EXT can be any format (png, jpg etc.) that can be read by the Python
Imaging Library.

License
-------

pixel2svg is licensed under the GPL. See the file COPYING for details.

Author
------

(c) Florian Berger
"""

distutils.core.setup(name = "pixel2svg",
                     version = pixel2svg.VERSION,
                     author = "Florian Berger",
                     author_email = "fberger@florian-berger.de",
                     url = "http://florian-berger.de/en/software/pixel2svg/",
                     description = "pixel2svg - Convert pixel art to SVG",
                     long_description = LONG_DESCRIPTION,
                     license = "GPL",
                     py_modules = [],
                     packages = [],
                     requires = ["PIL", "svgwrite"],
                     provides = ["pixel2svg"],
                     scripts = ["pixel2svg.py"],
                     data_files = [("", ["COPYING"])])
