using System.Collections.Generic;

public class PriorityQueue<T>
{
    private List<(T item, float priority)> elements = new List<(T, float)>();

    public int Count => elements.Count;

    public bool Empty => elements.Count == 0;

    public void Add(T item, float priority)
    {
        elements.Add((item, priority));
    }

    public T Get()
    {
        int bestIndex = 0;
        for (int i = 1; i < elements.Count; i++)
        {
            if (elements[i].priority < elements[bestIndex].priority)
                bestIndex = i;
        }
        T bestItem = elements[bestIndex].item;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }

    public bool Remove(T item)
    {
        int deleteIndex = -1;
        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i].item.Equals(item))
            {
                deleteIndex = i;
            }
        }
        if(deleteIndex == -1)
            return false;
        elements.RemoveAt(deleteIndex);
        return true;
    }
}
