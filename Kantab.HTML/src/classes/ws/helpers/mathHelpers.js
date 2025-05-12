export function expDecay(a, b, decay, delta) {
    return b+(a-b)*Math.exp(-decay*delta);
}

export function lerp(a, b, t){
    return a + (b-a) * t;
}

export function invLerp(a, b, v){
    return (v-a)/(b-a);
}

export function clamp(val, max, min){
    if (val > max) return max
    else if (val < min) return min
    else return val;
}