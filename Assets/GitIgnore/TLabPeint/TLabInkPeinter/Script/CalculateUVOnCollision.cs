using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculateUVOnCollision
{
    // -----------------------------------------------------------------
    // このプログラム, Meshが複雑なPlaneでだけはUVが計算出来ない
    // 平面メッシュの描画の仕方に問題があるのか?
    // 平面を傾けてみたら正しくuvを算出することができた
    // → mvp行列との計算をw成分を0で計算していたからだった

    // --------------------------------------------------------------------
    // このプログラム，GameObjectと Raycastで衝突判定が出来るときは不要
    // Collision系の衝突判定で Uv座標を計算するときには必須

    #region 点pがポリゴンの辺の上にあるかを内積で判定

    /// <summary>
    /// この関数で実際にpointが辺上にあるかを内積で判定する
    /// </summary>
    /// <param name="p">ObjectSpaceにおけるpoint</param>
    /// <param name="v1">ObjectSpaceにおけるポリゴンの頂点</param>
    /// <param name="v2">ObjectSpaceにおけるポリゴンの頂点</param>
    /// <returns>ExistPointOnTriangleEdgeに真偽を返す</returns>
    public static bool ExistPointOnEdge(Vector3 p, Vector3 v1, Vector3 v2)
    {
        //TOLERANCEは誤差の許容範囲
        //大きさ1のベクトル同士の内積をとっているので、1より大きくなることはない
        return 1 - 0.000001f < Vector3.Dot((v2 - p).normalized, (v2 - v1).normalized);
    }

    /// <summary>
    /// pointがポリゴンの辺上にあるかを色々な辺の組み合わせで判定する
    /// </summary>
    /// <param name="p">ObjectSpaceにおけるpoint</param>
    /// <param name="t1">ObjectSpaceにおけるポリゴンの頂点</param>
    /// <param name="t2">同上</param>
    /// <param name="t3">同上</param>
    /// <returns>辺の上にあったか無かったかを返す</returns>
    public static bool ExistPointOnTriangleEdge(Vector3 p, Vector3 t1, Vector3 t2, Vector3 t3)
    {
        if (ExistPointOnEdge(p, t1, t2) || ExistPointOnEdge(p, t2, t3) || ExistPointOnEdge(p, t3, t1))
        {
            //上の関数の実行結果を返す
            return true;
        }

        return false;
    }

    #endregion 点pがポリゴンの辺の上にあるかを内積で判定

    #region 他スクリプトから呼び出す関数

    /// <summary>
    /// 受け取った空間座標がポリゴンの表面に位置しているかを計算する
    /// </summary>
    /// <param name="point">World空間における衝突位置</param>
    /// <param name="meshFilter">point表面にあるかを判定したいObjectのmeshFilter</param>
    /// <param name="transform">point表面にあるかを判定したいObjectのtransfrom</param>
    /// <returns>計算したuv座標を返す</returns>
    public Vector2 CalculateUV(Vector3 point, MeshFilter meshFilter, Transform transform)
    {
        var mesh = meshFilter.sharedMesh;
        Vector2 uv = new Vector2(2, 2);

        // mesh.triangles : meshの頂点の並びの配列（{0, 1, 2, 0, ･･･}みたいなやつ）
        for (var i = 0; i < mesh.triangles.Length; i += 3)
        {
            #region 1.ある点pが与えられた3点において平面上に存在するか

            var index0 = i + 0;
            var index1 = i + 1;
            var index2 = i + 2;

            // mesh.vertices : meshのobject空間における位置
            var p1 = mesh.vertices[mesh.triangles[index0]];
            var p2 = mesh.vertices[mesh.triangles[index1]];
            var p3 = mesh.vertices[mesh.triangles[index2]];

            // objectに対する衝突地点のlocalな座標
            // pointはnullのときVector3.zeroを返してくる
            var p = transform.InverseTransformPoint(point);
            //Debug.Log(p + "," + point);

            var v1 = p2 - p1;
            var v2 = p3 - p1;
            var vp = p - p1;

            // 任意の点pと各ポリゴンの外積を求めて, 同じ方向を向いているかを調べる
            var nv = Vector3.Cross(v1, v2);
            var val = Vector3.Dot(nv, vp);

            var suc = -0.000001f < val && val < 0.000001f;

            #endregion 1.ある点pが与えられた3点において平面上に存在するか

            #region 2.同一平面上に存在する点pが三角形内部に存在するか

            if (!suc)
                continue;
            else
            {
                var a = Vector3.Cross(p1 - p3, p - p1).normalized;
                var b = Vector3.Cross(p2 - p1, p - p2).normalized;
                var c = Vector3.Cross(p3 - p2, p - p3).normalized;

                var d_ab = Vector3.Dot(a, b);
                var d_bc = Vector3.Dot(b, c);

                suc = 0.999f < d_ab && 0.999f < d_bc || ExistPointOnTriangleEdge(p, p1, p2, p3);
            }

            #endregion 2.同一平面上に存在する点pが三角形内部に存在するか

            #region 3.点pのUV座標を求める

            if (!suc)
                continue;
            else
            {
                // mesh.verticesに対応したuv座標
                var uv1 = mesh.uv[mesh.triangles[index0]];
                var uv2 = mesh.uv[mesh.triangles[index1]];
                var uv3 = mesh.uv[mesh.triangles[index2]];

                //PerspectiveCollect(透視投影を考慮したUV補間)
                Matrix4x4 mvp = Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix * transform.localToWorldMatrix;

                // 各点をProjectionSpaceへの変換(実際には左に掛けている((コンピュータなので横ベクトル)))
                // wはviewSpaceにおけるz座標
                // 色々解決したのでurl貼っておく
                // ベクトルのw成分を0で計算していたので、平行移動の行列が作用していなかった。
                // →これはつまりlocal to world変換が出来ていなかった
                /* https://esprog.hatenablog.com/entry/2016/10/09/062952 */

                Vector4 p1_p = mvp * new Vector4(p1.x, p1.y, p1.z, 1);
                Vector4 p2_p = mvp * new Vector4(p2.x, p2.y, p2.z, 1);
                Vector4 p3_p = mvp * new Vector4(p3.x, p3.y, p3.z, 1);
                Vector4 p_p = mvp * new Vector4(p.x, p.y, p.z, 1);

                Vector2 p1_n = new Vector2(p1_p.x, p1_p.y) / p1_p.w;
                Vector2 p2_n = new Vector2(p2_p.x, p2_p.y) / p2_p.w;
                Vector2 p3_n = new Vector2(p3_p.x, p3_p.y) / p3_p.w;
                Vector2 p_n = new Vector2(p_p.x, p_p.y) / p_p.w;
                // 頂点のなす三角形を点pにより3分割し、必要になる面積を計算
                var s = 0.5f * ((p2_n.x - p1_n.x) * (p3_n.y - p1_n.y) - (p2_n.y - p1_n.y) * (p3_n.x - p1_n.x));
                var s1 = 0.5f * ((p3_n.x - p_n.x) * (p1_n.y - p_n.y) - (p3_n.y - p_n.y) * (p1_n.x - p_n.x));
                var s2 = 0.5f * ((p1_n.x - p_n.x) * (p2_n.y - p_n.y) - (p1_n.y - p_n.y) * (p2_n.x - p_n.x));
                // 面積比からuvを補間
                var u = s1 / s;
                var v = s2 / s;
                var w = 1 / ((1 - u - v) * 1 / p1_p.w + u * 1 / p2_p.w + v * 1 / p3_p.w);
                uv = w * ((1 - u - v) * uv1 / p1_p.w + u * uv2 / p2_p.w + v * uv3 / p3_p.w);
                return uv;
            }
            #endregion 3.点pのUV座標を求める
        }
        return uv;
    }

    #endregion 他スクリプトから呼び出す関数
}
