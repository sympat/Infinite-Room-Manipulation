Shader "Custom/UVRepeatingShader"
{
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTexCeil ("Ceil Texture (RGB)", 2D) = "surface" {}
        _MainTexWall ("Wall Texture (RGB)", 2D) = "surface" {}
        _MainTexFloor ("Floor Texture", 2D) = "surface" {}
        _Scale ("Texture Scale", Float) = 0.1
        _DoorMin("Door Min", Vector) = (0, 0, 0, 0) // xyz - min
        _DoorMax("Door Max", Vector) = (0, 0, 0, 0) // xyz - max
    }

    SubShader {

    Tags { "RenderType"="Opaque" }

    CGPROGRAM
    #pragma surface surf Lambert
         
    struct Input {
        float3 worldNormal;
        float3 worldPos;
    };

    sampler2D _MainTexWall;
    sampler2D _MainTexCeil;
    sampler2D _MainTexFloor;
    
    float4 _DoorMin, _DoorMax;
    float4 _Color;
    float _Scale;

    void surf (Input IN, inout SurfaceOutput o) {
        float2 UV;
        fixed4 c;

        if(abs(IN.worldNormal.x)>0.5) {
            // Calculate a signed distance from the clipping volume.
            float3 offset;
            offset = IN.worldPos.xyz - _DoorMax.xyz;
            float outOfBounds = max(offset.x, max(offset.y, offset.z));
            offset = _DoorMin.xyz - IN.worldPos.xyz;
            outOfBounds = max(outOfBounds, max(offset.x, max(offset.y, offset.z)));

            // Reject fragments that are outside the clipping volume.
            clip(outOfBounds);

            UV = IN.worldPos.yz; // wall side
            c = tex2D(_MainTexWall, UV* _Scale); // use Wall texture (blue)
        } else if(abs(IN.worldNormal.z)>0.5) { 
            // Calculate a signed distance from the clipping volume.
            float3 offset;
            offset = IN.worldPos.xyz - _DoorMax.xyz;
            float outOfBounds = max(offset.x, max(offset.y, offset.z));
            offset = _DoorMin.xyz - IN.worldPos.xyz;
            outOfBounds = max(outOfBounds, max(offset.x, max(offset.y, offset.z)));

            // Reject fragments that are outside the clipping volume.
            clip(outOfBounds);

            UV = IN.worldPos.xy; // wall front 
            c = tex2D(_MainTexWall, UV* _Scale); // use Wall texture (green)
        } else if(IN.worldNormal.y > 0.5) {
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
