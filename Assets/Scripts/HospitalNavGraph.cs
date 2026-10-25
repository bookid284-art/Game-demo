using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 医院走廊的稀疏路点。关门的边不会被寻路使用。
/// </summary>
public class HospitalNavGraph : MonoBehaviour
{
    public List<string> names = new List<string>();
    public List<Vector3> nodes = new List<Vector3>();
    public List<int> linkA = new List<int>();
    public List<int> linkB = new List<int>();
    public List<bool> linkOpen = new List<bool>();

    public int AddNode(string nodeName, float x, float z)
    {
        names.Add(nodeName);
        nodes.Add(new Vector3(x, 0f, z));
        return nodes.Count - 1;
    }

    public void AddLink(int a, int b, bool open)
    {
        linkA.Add(a);
        linkB.Add(b);
        linkOpen.Add(open);
    }

    public void SetOpen(int a, int b, bool open)
    {
        for (int i = 0; i < linkA.Count; i++)
        {
            bool match = (linkA[i] == a && linkB[i] == b) || (linkA[i] == b && linkB[i] == a);
            if (match)
                linkOpen[i] = open;
        }
    }

    public Vector3 NodePosition(int index)
    {
        return nodes[index];
    }

    public int Nearest(Vector3 world)
    {
        int best = 0;
        float bestDistance = float.PositiveInfinity;
        for (int i = 0; i < nodes.Count; i++)
        {
            float dx = nodes[i].x - world.x;
            float dz = nodes[i].z - world.z;
            float distance = dx * dx + dz * dz;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = i;
            }
        }
        return best;
    }

    public List<int> PathToward(int start, int goal)
    {
        List<int> direct = FindPath(start, goal);
        if (direct != null)
            return direct;

        List<int> reachable = Flood(start);
        int best = start;
        float bestDistance = DistanceSquared(start, goal);
        for (int i = 0; i < reachable.Count; i++)
        {
            float distance = DistanceSquared(reachable[i], goal);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = reachable[i];
            }
        }

        if (best == start)
        {
            List<int> stay = new List<int>();
            stay.Add(start);
            return stay;
        }

        List<int> partial = FindPath(start, best);
        if (partial == null)
        {
            List<int> stay = new List<int>();
            stay.Add(start);
            return stay;
        }
        return partial;
    }

    public List<int> FindPath(int start, int goal)
    {
        int count = nodes.Count;
        if (count == 0 || start < 0 || goal < 0 || start >= count || goal >= count)
            return null;

        float[] cost = new float[count];
        int[] previous = new int[count];
        bool[] closed = new bool[count];
        for (int i = 0; i < count; i++)
        {
            cost[i] = float.PositiveInfinity;
            previous[i] = -1;
        }
        cost[start] = 0f;

        while (true)
        {
            int current = -1;
            float bestScore = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                if (closed[i] || float.IsPositiveInfinity(cost[i]))
                    continue;
                float score = cost[i] + Vector3.Distance(nodes[i], nodes[goal]);
                if (score < bestScore)
                {
                    bestScore = score;
                    current = i;
                }
            }

            if (current < 0)
                return null;
            if (current == goal)
                break;

            closed[current] = true;
            for (int edge = 0; edge < linkA.Count; edge++)
            {
                if (!linkOpen[edge])
                    continue;
                int next = -1;
                if (linkA[edge] == current)
                    next = linkB[edge];
                else if (linkB[edge] == current)
                    next = linkA[edge];
                if (next < 0)
                    continue;

                float nextCost = cost[current] + Vector3.Distance(nodes[current], nodes[next]);
                if (nextCost < cost[next])
                {
                    cost[next] = nextCost;
                    previous[next] = current;
                }
            }
        }

        List<int> path = new List<int>();
        int cursor = goal;
        while (cursor >= 0)
        {
            path.Add(cursor);
            if (cursor == start)
                break;
            cursor = previous[cursor];
        }
        path.Reverse();
        return path;
    }

    List<int> Flood(int start)
    {
        List<int> result = new List<int>();
        bool[] seen = new bool[nodes.Count];
        List<int> stack = new List<int>();
        stack.Add(start);
        seen[start] = true;
        while (stack.Count > 0)
        {
            int index = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);
            result.Add(index);
            for (int edge = 0; edge < linkA.Count; edge++)
            {
                if (!linkOpen[edge])
                    continue;
                int next = -1;
                if (linkA[edge] == index)
                    next = linkB[edge];
                else if (linkB[edge] == index)
                    next = linkA[edge];
                if (next >= 0 && !seen[next])
                {
                    seen[next] = true;
                    stack.Add(next);
                }
            }
        }
        return result;
    }

    float DistanceSquared(int a, int b)
    {
        float dx = nodes[a].x - nodes[b].x;
        float dz = nodes[a].z - nodes[b].z;
        return dx * dx + dz * dz;
    }
}
