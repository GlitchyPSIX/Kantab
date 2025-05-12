// input: 0-1
export function rgb2hsv (r, g, b) {
    let value = Math.max(r,g,b), chrominance = value - Math.min(r,g,b);
    // https://en.wikipedia.org/wiki/HSL_and_HSV#Conversion_RGB_to_HSV_used_commonly_in_software_programming
    let hue = chrominance && 
                ((value==r) ?
                    (g-b)/chrominance :
                        ((value==g) ?
                            2+(b-r)/chrominance
                                : 4+(r-g)/chrominance));
    return [60*(hue <0 ? hue + 6 : hue), (value && (chrominance/value)) * 100, (value * 100)];
    // https://stackoverflow.com/a/54070620
}

// output: 0-1
export function hsv2rgb(h,s,v){
    let f= (n,k=(n+h/60)%6) => v - v*s*Math.max( Math.min(k,4-k,1), 0);     
    return [Math.round(f(5) * 255),Math.round(f(3) * 255),Math.round(f(1) * 255)];  
}