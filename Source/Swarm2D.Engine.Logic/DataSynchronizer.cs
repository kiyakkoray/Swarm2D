/******************************************************************************
Copyright (c) 2015 Koray Kiyakoglu

http://www.swarm2d.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Swarm2D.Engine.Logic
{
    public static class DataSynchronizer
    {
        public static bool SynchronizeWithList<SourceListItemType>(IDataSynchronizerHandler destinationHandler, List<SourceListItemType> sourceList)
        {
            //TODO: what happens if new object on sourcelist some how allocated on a deleted objects address?
            //TODO: items must be in same order with sourceList

            bool anythingChanged = false;

            //first delete non exist items
            for (int i = destinationHandler.GetObjectCount() - 1; i >= 0; i--)
            {
                Object item = destinationHandler.GetObjectAtIndex(i);

                if (!sourceList.Contains((SourceListItemType)item))
                {
                    anythingChanged = true;
                    destinationHandler.RemoveObject(i, item);
                }
            }

            //add new items.
            for (int i = 0; i < sourceList.Count && i < 16 && destinationHandler.GetObjectCount() < 16; i++)
            {
                Object item = sourceList[i];

                if (!destinationHandler.ContainsItem(item))
                {
                    anythingChanged = true;

                    destinationHandler.AddNewItem(item);
                }
            }

            return anythingChanged;
        }
    }

    public interface IDataSynchronizerHandler
    {
        int GetObjectCount();
        bool ContainsItem(object newObject);
        object GetObjectAtIndex(int index);
        void AddNewItem(object newObject);
        void RemoveObject(int index, object deletedObject);
    }
}
