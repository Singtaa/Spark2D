import { h } from 'preact'
import { forwardRef } from 'preact/compat'
import { RenderTexture } from 'UnityEngine'

interface Props {
    class?: string
    rt: RenderTexture
}

export const TextureDisplay = forwardRef(({ rt, class: className }: Props, ref) => {
    return (
        <image class={className} image={rt} />
    )
})