// Downscale one PNG with Core Image (built into macOS, no installs needed).
//
// Usage: resize_image <input.png> <output.png> <size> [unsharpRadius] [unsharpIntensity]
//
// - Lanczos resample so the longest side == size.
// - Core Image filters in linear light, so thin dark lines don't wash out.
// - Light unsharp mask restores edge contrast lost in resampling.
// - Result is centered on a transparent size x size canvas, written as 8-bit RGBA sRGB PNG.

import CoreImage
import Foundation

func fail(_ message: String) -> Never {
    FileHandle.standardError.write((message + "\n").data(using: .utf8)!)
    exit(1)
}

let args = CommandLine.arguments
guard args.count >= 4, let size = Int(args[3]), size > 0 else {
    fail("usage: resize_image <input.png> <output.png> <size> [unsharpRadius] [unsharpIntensity]")
}
let inputURL = URL(fileURLWithPath: args[1])
let outputURL = URL(fileURLWithPath: args[2])
let unsharpRadius = args.count > 4 ? Double(args[4]) ?? 0.6 : 0.6
let unsharpIntensity = args.count > 5 ? Double(args[5]) ?? 0.6 : 0.6

guard let source = CIImage(contentsOf: inputURL) else { fail("could not read \(inputURL.path)") }
let extent = source.extent
let target = CGFloat(size)
let scale = target / max(extent.width, extent.height)

// Lanczos downscale.
guard let lanczos = CIFilter(name: "CILanczosScaleTransform") else { fail("CILanczosScaleTransform unavailable") }
lanczos.setValue(source, forKey: kCIInputImageKey)
lanczos.setValue(scale, forKey: kCIInputScaleKey)
lanczos.setValue(1.0, forKey: kCIInputAspectRatioKey)
guard var image = lanczos.outputImage else { fail("Lanczos scale failed") }

let outW = min(target, (extent.width * scale).rounded())
let outH = min(target, (extent.height * scale).rounded())
image = image.cropped(to: CGRect(x: image.extent.minX, y: image.extent.minY, width: outW, height: outH))

// Light unsharp mask (clamp first so the image border isn't darkened).
if unsharpRadius > 0 && unsharpIntensity > 0 {
    let bounds = image.extent
    guard let unsharp = CIFilter(name: "CIUnsharpMask") else { fail("CIUnsharpMask unavailable") }
    unsharp.setValue(image.clampedToExtent(), forKey: kCIInputImageKey)
    unsharp.setValue(unsharpRadius, forKey: kCIInputRadiusKey)
    unsharp.setValue(unsharpIntensity, forKey: kCIInputIntensityKey)
    guard let sharpened = unsharp.outputImage else { fail("unsharp failed") }
    image = sharpened.cropped(to: bounds)
}

// Center on a transparent square canvas using whole-pixel offsets.
let dx = ((target - outW) / 2).rounded(.down) - image.extent.minX
let dy = ((target - outH) / 2).rounded(.down) - image.extent.minY
let canvasRect = CGRect(x: 0, y: 0, width: target, height: target)
let canvas = CIImage(color: .clear).cropped(to: canvasRect)
let final = image.transformed(by: CGAffineTransform(translationX: dx, y: dy))
    .composited(over: canvas)
    .cropped(to: canvasRect)

guard let srgb = CGColorSpace(name: CGColorSpace.sRGB) else { fail("no sRGB color space") }
let context = CIContext(options: [.workingColorSpace: CGColorSpace(name: CGColorSpace.extendedLinearSRGB)!])
do {
    try context.writePNGRepresentation(of: final, to: outputURL, format: .RGBA8, colorSpace: srgb)
} catch {
    fail("could not write \(outputURL.path): \(error)")
}
