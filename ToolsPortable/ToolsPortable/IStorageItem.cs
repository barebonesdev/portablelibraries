using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable
{
    public interface IStorageItem<T>
    {
        void Save(string fileName, T data);

        T Load(string fileName);

        void RemoveDir(string dirPath);

        void Remove(string fileName);
    }
}
