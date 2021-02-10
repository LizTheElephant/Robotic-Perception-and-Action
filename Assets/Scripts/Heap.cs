using UnityEngine;
using System.Collections;
using System;

//Heap structure to optimize searching through the open set in pathfinding based on {@link https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp}
public class Heap<T> where T : IHeapItem<T>
{
    T[] _items;
    int _size;

    public Heap(int maxSize)
    {
        if (maxSize <= 0)
            throw new InvalidOperationException("Heap size must be > 0");
        _items = new T[maxSize+1];
    }

    public int Count
    {
        get
        {
            return _size;
        }
    }

    public bool Contains(T item)
    {
        return Equals(_items[item.HeapIndex], item);
    }

    public void Enqueue(T item)
    {
        _size++;
        _items[_size] = item;
        item.HeapIndex = _size;
        SortUp(item);
    }

    public T Dequeue()
    {
        T firstItem = _items[1];
        if (_size == 1)
        {
            _items[1] = default(T);
            _size = 0;
            return firstItem;
        }
        T oldLast = _items[_size];
        _items[1] = oldLast;
        oldLast.HeapIndex = 1;
        _items[_size] = default(T);
        _size--;
        
        SortDown(oldLast);
        return firstItem;
    }

    public void UpdateItem(T item)
    {
        int parentIdx = item.HeapIndex >> 1;
        if(parentIdx > 0 && item.CompareTo(_items[parentIdx]) > 0)
        {
            SortUp(item);
        }
        else
        {
            SortDown(item);
        }
    }

    private void SortDown(T item)
    {
        int swapIdx = item.HeapIndex;
        int leftChildIdx = 2 * swapIdx;

        if(leftChildIdx > _size)
            return;

        int rightChildIdx = leftChildIdx + 1;
        T leftChild = _items[leftChildIdx];
        if(leftChild.CompareTo(item) > 0)
        {
            // no right child
            if(rightChildIdx > _size)
            {
                item.HeapIndex = leftChildIdx;
                leftChild.HeapIndex = swapIdx;
                _items[swapIdx] = leftChild;
                _items[leftChildIdx] = item;
                return;
            }
            T rightChild = _items[rightChildIdx];
            if(leftChild.CompareTo(rightChild) > 0)
            {
                leftChild.HeapIndex = swapIdx;
                _items[swapIdx] = leftChild;
                swapIdx = leftChildIdx;
            }
            else
            {
                // right is even higher, move it up and continue
                rightChild.HeapIndex = swapIdx;
                _items[swapIdx] = rightChild;
                swapIdx = rightChildIdx;
            }
        }
        // no right-child
        else if(rightChildIdx > _size)
            return;
        else
        {
            T rightChild = _items[rightChildIdx];
            if(rightChild.CompareTo(item) > 0)
            {
                rightChild.HeapIndex = swapIdx;
                _items[swapIdx] = rightChild;
                swapIdx = rightChildIdx;
            }
            else
                return;
        }

            while(true)
            {
                leftChildIdx = 2 * swapIdx;

                // is leaf
                if(leftChildIdx > _size)
                {
                    item.HeapIndex = swapIdx;
                    _items[swapIdx] = item;
                    break;
                }

                rightChildIdx = leftChildIdx + 1;
                leftChild = _items[leftChildIdx];
                if(leftChild.CompareTo(item) > 0)
                {
                    if(rightChildIdx > _size)
                    {
                        item.HeapIndex = leftChildIdx;
                        leftChild.HeapIndex = swapIdx;
                        _items[swapIdx] = leftChild;
                        _items[leftChildIdx] = item;
                        break;
                    }
                    T rightChild = _items[rightChildIdx];
                    if(leftChild.CompareTo(rightChild) > 0)
                    {
                        leftChild.HeapIndex = swapIdx;
                        _items[swapIdx] = leftChild;
                        swapIdx = leftChildIdx;
                    }
                    else
                    {
                        rightChild.HeapIndex = swapIdx;
                        _items[swapIdx] = rightChild;
                        swapIdx = rightChildIdx;
                    }
                }
                // no right child
                else if(rightChildIdx > _size)
                {
                    item.HeapIndex = swapIdx;
                    _items[swapIdx] = item;
                    break;
                }
                else
                {
                    T rightChild = _items[rightChildIdx];
                    if(rightChild.CompareTo(item) > 0)
                    {
                        rightChild.HeapIndex = swapIdx;
                        _items[swapIdx] = rightChild;
                        swapIdx = rightChildIdx;
                    }
                    else
                    {
                        item.HeapIndex = swapIdx;
                        _items[swapIdx] = item;
                        break;
                    }
                }
            }
        }

    private void SortUp(T item)
    {
        if (item.HeapIndex > 1)
        {
            int parent = item.HeapIndex >> 1;
            T parentItem = _items[parent];

            if (parentItem.CompareTo(item) >= 0)
                return;
            
            _items[item.HeapIndex] = parentItem;
            parentItem.HeapIndex = item.HeapIndex;
            item.HeapIndex = parent;
        
            while (parent > 1)
            {
                parent >>= 1;
                parentItem = _items[parent];

                if (parentItem.CompareTo(item) >= 0)
                    break;

                _items[item.HeapIndex] = parentItem;
                parentItem.HeapIndex = item.HeapIndex;
                item.HeapIndex = parent;
            }
            _items[item.HeapIndex] = item;
        }
    }
}

public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex
    {
        get;
        set;
    }
}
