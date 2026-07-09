#ifndef LIBRARY_SPECULAR

#define PI 3.14159265359

// Calcula a refletância especular da superfície utilizando o modelo de Blinn-Phong.
vec3 specularReflectance(vec3 fresnel, vec3 normal, vec3 halfAngle, vec3 viewDirection, vec3 LightDirection, float roughness)
{
    // Mapeamento de Rugosidade para Expoente de Polimento (Shininess)
    float shininess = 2.0 / (roughness * roughness + 0.0001) - 2.0;
    shininess = max(shininess, 1.0);
    
    // Fator de normalização para garantir a conservação de energia (evita que o brilho suma)
    float normalization = (shininess + 2.0) / (2.0 * PI);

    // Termo de Distribuição de Normais (D) - Alinhamento das microfacetas
    float NdotH = max(dot(normal, halfAngle), 0.0);
    float D = pow(NdotH, shininess) * normalization;

    // Produtos escalares para os termos de Visibilidade e Geometria
    float NdotL = max(dot(normal, LightDirection), 0.0);
    float NdotV = max(dot(normal, viewDirection), 0.0);
    float VdotH = max(dot(viewDirection, halfAngle), 0.0);

    // Termo Geométrico de Cook-Torrance (G)
    float G = min(1.0, min((2.0 * NdotH * NdotV) / (VdotH + 0.0001), (2.0 * NdotH * NdotL) / (VdotH + 0.0001)));
    
    // BRDF Especular Completa
    vec3 blinnPhong = (D * fresnel * G) / (4.0 * max(NdotL, 0.001) * max(NdotV, 0.001));

    return blinnPhong;
}

#define LIBRARY_SPECULAR
#endif
