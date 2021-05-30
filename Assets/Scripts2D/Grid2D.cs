/* Adapted from https://github.com/vazgriz/DungeonGenerator

Copyright (c) 2015-2019 Simon Zeni (simonzeni@gmail.com)


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
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid2D<T> {
    T[] data;

    public Vector2Int Size { get; private set; }
    public Vector2Int Offset { get; set; }

    public Grid2D(Vector2Int size, Vector2Int offset) {
        Size = size;
        Offset = offset;

        data = new T[size.x * size.y];
    }

    public int GetIndex(Vector2Int pos) {
        return pos.x + (Size.x * pos.y);
    }

    public bool InBounds(Vector2Int pos) {
        return new RectInt(Vector2Int.zero, Size).Contains(pos + Offset);
    }

    public T this[int x, int y] {
        get {
            return this[new Vector2Int(x, y)];
        }
        set {
            this[new Vector2Int(x, y)] = value;
        }
    }

    public T this[Vector2Int pos] {
        get {
            pos += Offset;
            return data[GetIndex(pos)];
        }
        set {
            pos += Offset;
            data[GetIndex(pos)] = value;
        }
    }
}
