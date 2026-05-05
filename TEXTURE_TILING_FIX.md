# Correção do Problema de Textura - Repetição vs. Esticamento

## Problema Identificado

O `tex_scale` não estava tendo efeito nas texturas das paredes e comodo "room" porque:
1. A escala era aplicada **durante o carregamento** dos modelos (no Python)
2. Os dados já estavam enviados para a GPU e não podiam ser alterados em tempo real
3. O shader não recebia o parâmetro `tex_scale`, então ele não podia fazer a escala das coordenadas UV

## Solução Implementada

Foram feitas três alterações principais:

### 1. **Vertex Shader** (`vertex_shader.vs`)
- Adicionado uniforme `tex_scale` 
- As coordenadas de textura agora são multiplicadas pelo `tex_scale` no shader
- Isso permite que a escala seja aplicada em tempo real durante a renderização

```glsl
uniform float tex_scale;        

void main(){
	gl_Position = projection * view * model * vec4(position,1.0);
	out_texture = texture_coord * tex_scale;  // ← Multiplicação feita aqui
}
```

### 2. **Python - Carregamento de Modelos** (`code.ipynb`)
- Removida a multiplicação de `tex_scale` na função `load_obj_and_texture`
- As coordenadas UV agora são carregadas diretamente do arquivo OBJ sem modificação
- Isso permite usar UVs padrão (entre 0 e 1) que se comportam corretamente com `GL_REPEAT`

```python
# ANTES (não funcionava):
u = float(modelo['texture'][texture_id - 1][0]) * tex_scale  # ❌

# DEPOIS (funciona):
u = float(modelo['texture'][texture_id - 1][0])  # ✅
```

### 3. **Python - Função `draw_model`** (`code.ipynb`)
- Armazenado `tex_scale` no dicionário `game_assets` ao registrar modelos
- A função `draw_model` agora passa `tex_scale` como uniforme para o shader

```python
def draw_model(name, ...):
    asset = game_assets[name]
    ...
    # Passar o tex_scale como uniform para o shader
    loc_tex_scale = glGetUniformLocation(program, "tex_scale")
    glUniform1f(loc_tex_scale, asset['tex_scale'])
    ...
```

## Resultado

Agora as texturas funcionam corretamente:
- ✅ **Paredes (wall)**: `tex_scale=30.0` → textura se repete 30 vezes
- ✅ **Comodo (room)**: `tex_scale=15.0` → textura se repete 15 vezes  
- ✅ **Piso interno (floor_int)**: `tex_scale=5.0` → textura se repete 5 vezes

## Como Modificar os Valores

Para ajustar o quanto a textura repete, edite os valores em `register_model`:

```python
register_model('wall', 'objects/wall/wall.obj', 'objects/wall/texture.png', tex_scale=30.0)  # Aumentar/diminuir 30.0
register_model('room', 'objects/room/room.obj', 'objects/wall/texture.png', tex_scale=15.0)  # Aumentar/diminuir 15.0
```

- **Valores maiores** → mais repetições, padrão mais pequeno
- **Valores menores** → menos repetições, padrão mais grande
- **Valor 1.0** → uma única aplicação da textura

## Vantagem da Solução

Ao usar o shader para aplicar a escala:
- 🎯 A mudança é em tempo real e eficiente
- 🎯 Não requer recarregar os dados da GPU
- 🎯 É possível até animar `tex_scale` se necessário no futuro
- 🎯 As coordenadas UV originais do OBJ são preservadas (0 a 1)
