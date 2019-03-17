#!Z:\Applications\Python\python.exe

"""pixel2svg - Convert pixel art to SVG

   Copyright 2011 Florian Berger <fberger@florian-berger.de>
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

# Work started on Thu Jul 21 2011.

# Python 3
#
#import argparse

# Python 2
#
import optparse

import PIL.Image
import svgwrite
import os.path

VERSION = "0.3.0"

if __name__ == "__main__":


    # Notes for Python 3, once PIL supports it
    #
    #argument_parser = argparse.ArgumentParser(description="Convert pixel art to SVG")
    #
    #argument_parser.add_argument("IMAGEFILE",
    #                             help = "The image file to convert")
    #
    #argument_parser.add_argument("--overlap",
    #                             action = "store_true",
    #                             help = "If given, overlap vector squares by 1px")
    #
    #argument_parser.add_argument("--version",
    #                             action = "version",
    #                             version = VERSION,
    #                             help = "Display the program version")
    #
    #arguments = argument_parser.parse_args()

    argument_parser = optparse.OptionParser(description="Convert pixel art to SVG",
                                            usage = "pixel2svg [--overlap] IMAGEFILE",
                                            version = VERSION)

    argument_parser.add_option("--squaresize",
                               type = "int",
                               default = 40,
                               help = "Width and height of vector squares in pixels, default: 40")

    argument_parser.add_option("--overlap",
                               action = "store_const",
                               const = 1,
                               default = 0,
                               help = "If given, overlap vector squares by 1px")

    arguments, positional = argument_parser.parse_args()

    if not len(positional):

        argument_parser.print_help()

        raise SystemExit

    print("pixel2svg {0}".format(VERSION))
    print("Reading image file '{0}'".format(positional[0]))

    image = PIL.Image.open(positional[0])

    print("Converting image to RGBA")

    image = image.convert("RGBA")

    (width, height) = image.size

    print("Image is {0}x{1}".format(width, height))

    rgb_values = list(image.getdata())

    print("Read {0} pixels".format(len(rgb_values)))

    svgdoc = svgwrite.Drawing(filename = os.path.splitext(positional[0])[0] + ".svg",
                              size = ("{0}px".format(width * arguments.squaresize),
                                      "{0}px".format(height * arguments.squaresize)))

    rowcount = 0

    print("Will use an square overlap of {0}px".format(arguments.overlap))

    while rowcount < height:

        print("Processing pixel row {0} of {1}".format(rowcount + 1, height))

        colcount = 0

        while colcount < width:

            rgb_tuple = rgb_values.pop(0)

            # Omit transparent pixels
            #
            if rgb_tuple[3] > 0:

                # If --overlap is given, use a slight overlap to prevent
                # inaccurate SVG rendering
                #
                svgdoc.add(svgdoc.rect(insert = ("{0}px".format(colcount * arguments.squaresize),
                                                 "{0}px".format(rowcount * arguments.squaresize)),
                                       size = ("{0}px".format(arguments.squaresize + arguments.overlap),
                                               "{0}px".format(arguments.squaresize + arguments.overlap)),
                                       fill = svgwrite.rgb(rgb_tuple[0],
                                                           rgb_tuple[1],
                                                           rgb_tuple[2])))

            colcount = colcount + 1

        rowcount = rowcount + 1

    print("Saving SVG to '{0}'".format(svgdoc.filename))

    svgdoc.save()

    print("Operation finished. Have fun with your SVG.")
