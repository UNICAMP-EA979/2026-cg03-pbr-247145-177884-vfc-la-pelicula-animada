#version 330 core

#include "light.glsl"
#include "fresnel.glsl"
#include "diffuse.glsl"
#include "specular.glsl"

#define MAX_LIGHT 10
#define PI 3.14159265359

// Adicione luz ambiente ao modelo de sombreamento

in vec3 worldPosition;
in vec3 worldNormal;

out vec4 FragColor;

uniform Light lights[MAX_LIGHT];
uniform vec3 ambientColor;
uniform vec3 cameraPos;

void main()
{
    // Calcule a normal do fragmento
    vec3 worldNormalNormalized = normalize(worldNormal);

    // Calcule a direção de visualização (saindo do ponto)
    vec3 viewDirection = normalize(cameraPos - worldPosition);

    vec3 baseColor = vec3(0.5, 0.2, 0.5);
    float metallic = 0.0;
    float roughness = 0.25;

    vec3 color = vec3(0);

    vec3 ambientLightContribution = ambientColor * baseColor;

    for(int i = 0; i < MAX_LIGHT; i++)
    {
        Light light = lights[i];
        if(light.type == LIGHT_UNSET)
        {
            break;
        }

        //Calcule dados da luz (atenuação, cor, direção)
        float attenuation = computeLightAttenuation(light, worldPosition);
        vec3 lightColor = light.color * light.intensity;
        vec3 lightDirection = computeLightDirection(light, worldPosition);

        //Calcule o half-angle
        vec3 halfAngle = normalize(lightDirection + viewDirection);

        // F0 para Fresnel (dielétricos: 0.04, metais: baseColor)
        vec3 F0 = mix(vec3(0.04), baseColor, metallic);

        // Fresnel de Schlick
        vec3 fresnel = fresnelReflectance(baseColor, metallic, halfAngle, lightDirection);

        // Difusa (Lambert) com conservação de energia
        vec3 diffuse = (1.0 - metallic) * baseColor / PI;

        // Especular (Blinn-Phong)
        vec3 specular = specularReflectance(fresnel, worldNormalNormalized, halfAngle,
                                            viewDirection, lightDirection, roughness);

        vec3 reflectance = (1.0 - fresnel) * diffuse + fresnel * specular;

        float NdotL = max(dot(worldNormalNormalized, lightDirection), 0.0);

        // Contribuição da luz atual
        vec3 lightContribution = reflectance * lightColor * NdotL * attenuation;
        color += lightContribution;
    }

    FragColor = vec4(ambientLightContribution + color, 1.0);
}
