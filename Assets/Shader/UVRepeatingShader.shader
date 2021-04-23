Shader "Custom/UVRepeatingShader"
{
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTexCeil ("Ceil Texture (RGB)", 2D) = "surface" {}
        _MainTexWall ("Wall Texture (RGB)", 2D) = "surface" {}
        _MainTexFloor ("Floor Texture", 2D) = "surface" {}
        _Scale ("Texture Scale", Float) = 0.1
        DigonalPos1("Door Digonal Position 1", Vector) = (0, 0, 0, 0)
        DigonalPos2("Door Digonal Position 2", Vector) = (0, 0, 0, 0)
    }

    SubShader {

    Tags { "RenderType"="Opaque" }

    CGPROGRAM
    #pragma surface surf Lambert

    sampler2D _MainTexWall;
    sampler2D _MainTexCeil;
    sampler2D _MainTexFloor;
    float4 DigonalPos1, DigonalPos2;
    float4 _Color;
    float _Scale;

    struct Input {
        float3 worldNormal;
        float3 worldPos;
    };

    void surf (Input IN, inout SurfaceOutput o) {
        float2 UV;
        fixed4 c;
        float3 localPos = mul(unity_WorldToObject, float4(IN.worldPos, 1));

        // if(abs(IN.worldNormal.x)>0.5) {
        //     // Calculate a signed distance from the clipping volume.
        //     float3 offset;
        //     offset = IN.worldPos.xyz - _DoorMax.xyz;
        //     float outOfBounds = max(offset.x, max(offset.y, offset.z));
        //     offset = _DoorMin.xyz - IN.worldPos.xyz;
        //     outOfBounds = max(outOfBounds, max(offset.x, max(offset.y, offset.z)));

        //     // Reject fragments that are outside the clipping volume.
        //     clip(outOfBounds);

        //     UV = IN.worldPos.yz; // wall side
        //     c = tex2D(_MainTexWall, UV* _Scale); // use Wall texture (blue)
        // } else if(abs(IN.worldNormal.z)>0.5) { 
        //     // Calculate a signed distance from the clipping volume.
        //     float3 offset;
        //     offset = IN.worldPos.xyz - _DoorMax.xyz;
        //     float outOfBounds = max(offset.x, max(offset.y, offset.z));
        //     offset = _DoorMin.xyz - IN.worldPos.xyz;
        //     outOfBounds = max(outOfBounds, max(offset.x, max(offset.y, offset.z)));

        //     // Reject fragments that are outside the clipping volume.
        //     clip(outOfBounds);

        //     UV = IN.worldPos.xy; // wall front 
        //     c = tex2D(_MainTexWall, UV* _Scale); // use Wall texture (green)
        // } 

        if(abs(IN.worldNormal.x)>0.5 || abs(IN.worldNormal.z)>0.5) {
            // Calculate min and max position from the clipping volume.
            float xMax = (DigonalPos2.x >= DigonalPos1.x) ? DigonalPos2.x : DigonalPos1.x;
            float xMin = (DigonalPos2.x <= DigonalPos1.x) ? DigonalPos2.x : DigonalPos1.x;
            float yMax = (DigonalPos2.y >= DigonalPos1.y) ? DigonalPos2.y : DigonalPos1.y;
            float yMin = (DigonalPos2.y <= DigonalPos1.y) ? DigonalPos2.y : DigonalPos1.y;
            float zMax = (DigonalPos2.z >= DigonalPos1.z) ? DigonalPos2.z : DigonalPos1.z;
            float zMin = (DigonalPos2.z <= DigonalPos1.z) ? DigonalPos2.z : DigonalPos1.z;

            if((localPos.x >= xMin && localPos.x <= xMax) && (localPos.y >= yMin && localPos.y <= yMax) && (localPos.z >= zMin && localPos.z <= zMax))
                clip(-1); // discard this texel

            UV = IN.worldPos.yz; // wall side
            if(abs(IN.worldNormal.z)>0.5) UV = IN.worldPos.xy; // TODO: texture does not sync if we use IN.worldPos.xy
            c = tex2D(_MainTexWall, UV* _Scale); // use Wall texture (blue)
        }
        else if(IN.worldNormal.y > 0.5) {
            UV = IN.worldPos.xz; // floor
            c = tex2D(_MainTexFloor, UV* _Scale); // use Floor texture (red)
        } else {
            UV = IN.worldPos.xz; // ceil
            c = tex2D(_MainTexCeil, UV* _Scale); // use Ceil texture (red)
        }

        o.Albedo = c.rgb * _Color;
    }

    ENDCG
    }

    Fallback "VertexLit"
}
