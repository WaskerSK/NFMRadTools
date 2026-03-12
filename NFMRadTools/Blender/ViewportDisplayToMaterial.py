import bpy
for mat in bpy.data.materials:
    if not mat.use_nodes:
        continue
    nodes = mat.node_tree.nodes
    principled = None
    for node in nodes:
        if node.type == 'BSDF_PRINCIPLED':
            principled = node
            break
        if principled is None:
            continue
        
    color = mat.diffuse_color
    metallic = mat.metallic
    roughness = mat.roughness
    
    principled.inputs["Base Color"].default_value = color
    principled.inputs["Metallic"].default_value = metallic
    principled.inputs["Roughness"].default_value = roughness
    
print("Copied viewport display properties to shader surface.")